import sys

def patch(version):
    ASSEMBLY_INFO_FILE = "../Properties/AssemblyInfo.cs";
    print("Patching file: " + ASSEMBLY_INFO_FILE)
    newfile = "";
    with open(ASSEMBLY_INFO_FILE, "r") as f:
        for line in f:
            changed = False
            if line.startswith("[assembly: AssemblyVersion("):
                newline = '[assembly: AssemblyVersion("{}.0")]\n'.format(version)
                changed = True
            elif line.startswith("[assembly: AssemblyFileVersion("):
                newline = '[assembly: AssemblyFileVersion("{}.0")]\n'.format(version)
                changed = True
            else:
                newline = line
                
            if changed:
                print("Old line:\n{}New line:\n{}".format(line, newline))
            
            newfile += newline
            
    with open(ASSEMBLY_INFO_FILE, "w") as f:
        f.write(newfile)
        
                
    WXS_FILE = "../GolemUISetup/GolemUI.wxs";
    print("Patching file: " + WXS_FILE)
    newfile = "";
    with open(WXS_FILE, "r") as f:
        for line in f:
            changed = False
            if line.startswith("<?define Version="):
                newline = '<?define Version="{}"?>\n'.format(version)
                changed = True
            else:
                newline = line
                
            if changed:
                print("Old line:\n{}New line:\n{}".format(line, newline))
            
            newfile += newline
            
    with open(WXS_FILE, "w") as f:
        f.write(newfile)  


if len(sys.argv) == 1:
    print("Provide version as first argument in format 0.9.9\n python SetVersion.py ")
else:
    patch(sys.argv[1])
    



          
            


