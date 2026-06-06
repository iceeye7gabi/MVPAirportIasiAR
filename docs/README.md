# Documentation (LaTeX)

Technical documentation for colleagues: product idea, architecture, and implementation.

## Files

| File | Description |
|------|-------------|
| `documentation.tex` | Main LaTeX source |

## Compile to PDF

### Option 1 — Local LaTeX (recommended)

Requires a TeX distribution (TeX Live, MacTeX, or MiKTeX).

```bash
cd docs
pdflatex documentation.tex
pdflatex documentation.tex
```

Run `pdflatex` twice so the table of contents and references resolve.

Output: `documentation.pdf`

### Option 2 — latexmk

```bash
cd docs
latexmk -pdf documentation.tex
```

### Option 3 — Overleaf

1. Create a new project on [Overleaf](https://www.overleaf.com).
2. Upload `documentation.tex`.
3. Set compiler to **pdfLaTeX**.
4. Recompile.

## Contents overview

The PDF is split into two parts:

**Part I — Business Proposition**
- Executive summary
- The problem (airports under reconstruction, stakeholder pain points)
- Our solution (what the app is and what it solves)
- Passenger and staff capabilities
- Value proposition and differentiation
- Compliance and trust
- Hackathon scope and demo scenarios
- Vision for real deployment

**Part II — Technical Implementation**
- Unity architecture and module descriptions
- JSON map schema and pathfinding
- AR navigation, chatbot, staff mode
- Setup/build instructions and testing
