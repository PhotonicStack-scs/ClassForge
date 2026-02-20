import pathlib
BASE = 'C:/Users/Martin Damm Laupstad/source/repos/ClassForge/classforge-web'
def w(rel, content):
    p = pathlib.Path(BASE) / rel
    p.parent.mkdir(parents=True, exist_ok=True)
    p.write_text(content, encoding="utf-8")
    print("W:", rel)

