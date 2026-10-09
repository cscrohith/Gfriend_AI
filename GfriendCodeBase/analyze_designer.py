import os

filepath = r"GFriendUI\MainForm.Designer.cs"

if os.path.exists(filepath):
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()

    # Look for target device related code
    if 'comboBoxTargetDevice' in content:
        print("✓ FOUND: comboBoxTargetDevice")
        # Find the line
        lines = content.split('\n')
        for i, line in enumerate(lines):
            if 'comboBoxTargetDevice' in line:
                print(f"  Line {i+1}: {line.strip()}")
    else:
        print("✗ NOT FOUND: comboBoxTargetDevice")

    if 'labelTargetDevice' in content:
        print("\n✓ FOUND: labelTargetDevice")
        lines = content.split('\n')
        for i, line in enumerate(lines):
            if 'labelTargetDevice' in line and 'new' in line:
                print(f"  Line {i+1}: {line.strip()}")
                break
    else:
        print("\n✗ NOT FOUND: labelTargetDevice")

    print(f"\nFile size: {len(content)} characters")
    print(f"Total lines: {len(content.split(chr(10)))}")
else:
    print(f"✗ File not found: {filepath}")
