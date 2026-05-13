#!/usr/bin/env bash
# Build a two-column PDF from the FlashForth quick-reference markdown.
# Requires: pandoc, basictex (or mactex). BasicTeX installs xelatex
# at /Library/TeX/texbin which we add to PATH explicitly so this
# script works regardless of which shell the user normally runs.

set -euo pipefail

export PATH="/Library/TeX/texbin:$PATH"

cd "$(dirname "$0")"

SRC="flashforth5_quick_ref_AVR.md"
OUT="flashforth5_quick_ref_AVR.pdf"

pandoc "$SRC" \
  -o "$OUT" \
  --pdf-engine=xelatex \
  --from=markdown-tex_math_dollars-raw_tex \
  -H header.tex \
  -V documentclass=extarticle \
  -V classoption=twocolumn \
  -V geometry:margin=0.4in \
  -V fontsize=9pt \
  -V colorlinks=true

echo "Built $OUT"
