import base64
bang = chr(33)
def add(path, lines):
    txt = "
".join(lines) + "
"
    b64 = base64.b64encode(txt.encode()).decode()
    with open("w.py","a") as wf:
        wf.write("w(r" + chr(34) + path + chr(34) + "," + chr(34) + b64 + chr(34) + ")
")
add("C:/Users/Martin Damm Laupstad/source/repos/ClassForge/classforge-web/src/app/[locale]/(app)/users/page.tsx", [
  chr(34)*3,
