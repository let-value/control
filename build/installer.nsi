!include "MUI2.nsh"

Name "Control"
OutFile "control-setup.exe"

InstallDir "$APPDATA\Control"

InstallDirRegKey HKCU "Software\Control" ""

RequestExecutionLevel highest

!define MUI_ABORTWARNING

!insertmacro MUI_PAGE_COMPONENTS
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES

!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES

!insertmacro MUI_LANGUAGE "English"
!insertmacro MUI_LANGUAGE "Russian"

Section "Control" SecInstall
  SetOutPath "$INSTDIR"
  File /r ".\*"
  WriteRegStr HKCU "Software\Control" "" $INSTDIR
  WriteUninstaller "$INSTDIR\Uninstall.exe"
  ExecWait 'sc create Control start= delayed-auto binPath= "\"$INSTDIR\Control.exe\" -path.home \"$INSTDIR\" -path.data C:\ProgramData\Control -path.logs C:\ProgramData\Control\logs"'
  ExecWait 'net start Control'
SectionEnd

Section "Shortcut"
  CreateShortCut "$DESKTOP\Control.lnk" "$INSTDIR\Control.exe"
SectionEnd

Section "Uninstall"
  ExecWait 'net stop Control'
  ExecWait 'sc delete Control'
  Delete "$INSTDIR\Uninstall.exe"
  RMDir "$INSTDIR"
  DeleteRegKey /ifempty HKCU "Software\Control"
SectionEnd

Function .onInit
  SectionSetFlags 0 17
FunctionEnd