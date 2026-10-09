# -*- mode: python ; coding: utf-8 -*-

from PyInstaller.utils.hooks import collect_all
import os
import glob
import site

# Collect all pywin32 modules and DLLs
datas_pywin32, binaries_pywin32, hiddenimports_pywin32 = collect_all('pywin32')
datas_win32, binaries_win32, hiddenimports_win32 = collect_all('win32')

# Manually add pywin32_system32 DLLs - these are critical for win32print to work
pywin32_system32_path = None
for sp in site.getsitepackages():
    candidate = os.path.join(sp, 'pywin32_system32')
    if os.path.exists(candidate):
        pywin32_system32_path = candidate
        break

pywin32_system32_dlls = []
if pywin32_system32_path:
    for dll in glob.glob(os.path.join(pywin32_system32_path, '*.dll')):
        # Add DLLs to root of bundle so they can be found
        pywin32_system32_dlls.append((dll, '.'))

a = Analysis(
    ['print_load_tester.py'],
    pathex=[],
    binaries=binaries_pywin32 + binaries_win32 + pywin32_system32_dlls,
    datas=datas_pywin32 + datas_win32,
    hiddenimports=[
        'win32print',
        'win32api',
        'win32con',
        'pywintypes',
        'win32timezone',
        'PIL',
        'PIL.Image',
        'PIL.ImageWin',
        'img2pdf',
        'fitz',
        'docx',
        'openpyxl'
    ] + hiddenimports_pywin32 + hiddenimports_win32,
    hookspath=[],
    hooksconfig={},
    runtime_hooks=[],
    excludes=[],
    noarchive=False,
    optimize=0,
)
pyz = PYZ(a.pure)

exe = EXE(
    pyz,
    a.scripts,
    a.binaries,
    a.datas,
    [],
    name='PrintLoadTester',
    debug=False,
    bootloader_ignore_signals=False,
    strip=False,
    upx=True,
    upx_exclude=[],
    runtime_tmpdir=None,
    console=False,
    disable_windowed_traceback=False,
    argv_emulation=False,
    target_arch=None,
    codesign_identity=None,
    entitlements_file=None,
    icon='Load.ico',
)
