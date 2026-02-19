import pathlib
BASE="C:/Users/Martin Damm Laupstad/source/repos/ClassForge/classforge-web"
def w(rel,c):
    p=pathlib.Path(BASE)/rel
    p.parent.mkdir(parents=True,exist_ok=True)
    p.write_text(c,encoding="utf-8")
    print("W:",rel)

# --- time-structure page ---
lines=[]
lines.append(chr(34)+"use client"+chr(34)+chr(59))
lines.append("")
