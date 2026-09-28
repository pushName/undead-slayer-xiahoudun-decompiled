#!/usr/bin/env bash
# 修正仓库中 Windows 非法文件名，并把每条重命名记录到日志
#
# 用法:
#   ./fix-names.sh           # 预演（不会改文件，但会生成日志供查看）
#   ./fix-names.sh --apply   # 实际执行重命名
#
# 产出:
#   rename-log.tsv       Tab 分隔，字段: 原路径 / 新路径 / 命中的字符 / 替换规则
#   RENAME-REPORT.md     人类可读的 Markdown 汇总（含时间戳、统计）

set -euo pipefail

APPLY=0
[[ "${1:-}" == "--apply" ]] && APPLY=1

LOG_TSV="rename-log.tsv"
LOG_MD="RENAME-REPORT.md"
TS_NOW="$(date -u '+%Y-%m-%d %H:%M:%S UTC')"
REPO_ROOT="$(pwd)"

# 是否命中某个非法字符，命中就返回“字符 -> 替换”描述，否则为空
reason_for() {
  local s="$1"
  local reasons=()
  [[ "$s" == *"<"* ]] && reasons+=("'<' -> _lt_")
  [[ "$s" == *">"* ]] && reasons+=("'>' -> _gt_")
  [[ "$s" == *":"* ]] && reasons+=("':' -> _co_")
  [[ "$s" == *'"'* ]] && reasons+=('"\"" -> _qt_')
  [[ "$s" == *"|"* ]] && reasons+=("'|' -> _pi_")
  [[ "$s" == *"?"* ]] && reasons+=("'?' -> _qm_")
  [[ "$s" == *"*"* ]] && reasons+=("'*' -> _st_")
  [[ "$s" == *"\\"* ]] && reasons+=("'\\' -> _bs_")
  [[ "$s" == *"/"* ]] && reasons+=("'/' -> _sl_")   # 文件名里出现 '/' 是异常，兜底
  if [[ "$s" == \$* ]]; then
    reasons+=("前导 '\$' -> _dollar_")
  fi
  local out=""
  for r in "${reasons[@]}"; do
    [[ -n "$out" ]] && out+="; "
    out+="$r"
  done
  printf '%s' "$out"
}

map_char() {
  local s="$1"
  s="${s//</_lt_}"
  s="${s//>/_gt_}"
  s="${s//:/_co_}"
  s="${s//\"/_qt_}"
  s="${s//|/_pi_}"
  s="${s//\?/_qm_}"
  s="${s//\*/_st_}"
  s="${s//\\/_bs_}"
  s="${s////_sl_}"
  if [[ "$s" == \$* ]]; then
    s="_dollar_${s:1}"
  fi
  printf '%s' "$s"
}

# ---- 扫描并收集 ----
# 用数组保存所有条目，字段: "old<TAB>new<TAB>reason"
entries=()
while IFS= read -r -d '' f; do
  dir=$(dirname "$f")
  base=$(basename "$f")
  newbase=$(map_char "$base")
  [[ "$base" == "$newbase" ]] && continue
  newpath="$dir/$newbase"

  reason=$(reason_for "$base")
  # 记录相对路径（去掉开头的 './'）
  rel_old="${f#./}"
  rel_new="${newpath#./}"
  entries+=("$rel_old"$'\t'"$rel_new"$'\t'"$reason")
done < <(find . -path ./.git -prune -o -print0 | grep -zE '[<>:"|?*\\]' || true)

total=${#entries[@]}

# ---- 预演输出 ----
if [[ $total -gt 0 ]]; then
  echo "== 计划重命名 =="
  for e in "${entries[@]}"; do
    IFS=$'\t' read -r o n r <<<"$e"
    printf '  %s\n  -> %s\n     原因: %s\n' "$o" "$n" "$r"
  done
  echo
fi

# ---- 写入 TSV 日志 ----
{
  printf '# rename-log for %s\n' "$REPO_ROOT"
  printf '# generated at %s\n' "$TS_NOW"
  printf '# mode: %s\n' "$([[ $APPLY -eq 1 ]] && echo APPLY || echo DRY-RUN)"
  printf 'old_path\tnew_path\treason\n'
  for e in "${entries[@]}"; do
    printf '%s\n' "$e"
  done
} > "$LOG_TSV"

# ---- 写入 Markdown 报告 ----
{
  echo "# 非法文件名修正报告"
  echo
  echo "- 仓库: \`$REPO_ROOT\`"
  echo "- 生成时间: $TS_NOW"
  echo "- 模式: **$([[ $APPLY -eq 1 ]] && echo APPLY || echo DRY-RUN)**"
  echo "- 共 **$total** 个文件需要重命名"
  echo
  echo "## 规则"
  echo
  echo "| 非法字符 | 替换为 |"
  echo "| :--- | :--- |"
  echo "| \`<\` | \`_lt_\` |"
  echo "| \`>\` | \`_gt_\` |"
  echo "| \`:\` | \`_co_\` |"
  echo "| \`\"\` | \`_qt_\` |"
  echo "| \`\|\` | \`_pi_\` |"
  echo "| \`?\` | \`_qm_\` |"
  echo "| \`*\` | \`_st_\` |"
  echo "| \`\\\\\` | \`_bs_\` |"
  echo "| 前导 \`\$\` | \`_dollar_\` |"
  echo
  echo "## 明细"
  echo
  echo "| # | 原路径 | 新路径 | 原因 |"
  echo "| ---: | :--- | :--- | :--- |"
  i=0
  for e in "${entries[@]}"; do
    i=$((i+1))
    IFS=$'\t' read -r o n r <<<"$e"
    printf '| %d | `%s` | `%s` | %s |\n' "$i" "$o" "$n" "$r"
  done
} > "$LOG_MD"

# ---- 执行重命名 ----
if [[ $APPLY -eq 1 ]]; then
  done_count=0
  for e in "${entries[@]}"; do
    IFS=$'\t' read -r o n r <<<"$e"
    if [[ -e "$n" && "$n" != "$o" ]]; then
      echo "SKIP (target exists): $o -> $n"
      continue
    fi
    mv -n -- "$o" "$n"
    done_count=$((done_count+1))
  done
  echo "已重命名 $done_count 个文件。"
  echo "日志: $LOG_TSV"
  echo "报告: $LOG_MD"
  echo
  echo "下一步: git status 查看重命名，然后 git add -A && git commit"
else
  echo "Dry-run 完成，共 $total 个文件待重命名。"
  echo "日志: $LOG_TSV"
  echo "报告: $LOG_MD"
  echo "确认无误后运行: ./fix-names.sh --apply"
fi
