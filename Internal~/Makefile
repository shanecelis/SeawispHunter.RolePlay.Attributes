all: README.pdf

README.pdf: README.md
	pandoc -o README.pdf --pdf-engine=xelatex  README.md -V geometry:margin=1in --highlight-style espresso -V colorlinks=true
