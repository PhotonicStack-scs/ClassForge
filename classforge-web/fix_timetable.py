import base64, os
def w(p,b):
    os.makedirs(os.path.dirname(p),exist_ok=True)
    open(p,"wb").write(base64.b64decode(b))
    print("wrote:",p)
bt=chr(96)
bang=chr(33)
dollar=chr(36)
ob=chr(123)
cb=chr(125)
parts=[]
