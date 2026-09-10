"""Validate Navigation in an isolated Unity project, without the game or test-framework packages."""
import argparse
import json
import os
from pathlib import Path
import shutil
import subprocess
import sys
import tempfile


def main():
    sys.stdout.reconfigure(encoding='utf-8', errors='replace')
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--unity', required=True, help='Unity Editor executable')
    parser.add_argument('--ugui', help='Optional local com.unity.ugui package directory')
    parser.add_argument('--timeout', type=int, default=600)
    args = parser.parse_args()
    engine = Path(args.unity).resolve()
    if not engine.is_file():
        parser.error('Unity Editor was not found.')
    ugui = Path(args.ugui).resolve() if args.ugui else engine.parent / 'Data/Resources/PackageManager/BuiltInPackages/com.unity.ugui'
    if not (ugui / 'package.json').is_file():
        parser.error('Supply --ugui with a locally installed uGUI package directory.')
    module = Path(__file__).resolve().parents[1]
    project = Path(tempfile.mkdtemp(prefix='lunar-navigation-tests-'))
    navigation = project / 'Assets/Navigation'
    shutil.copytree(module / 'Runtime', navigation)
    shutil.copytree(module / 'Tests~/Runtime', navigation / 'Validation')
    shutil.copytree(module / 'Tests~/Editor', project / 'Assets/Editor')
    (project / 'Packages').mkdir()
    (project / 'ProjectSettings').mkdir()
    (project / 'Packages/manifest.json').write_text(json.dumps({'dependencies': {
        'com.unity.ugui': 'file:' + ugui.as_posix(),
        'com.unity.modules.jsonserialize': '1.0.0',
    }}, indent=2), encoding='utf-8')
    # Existing Fade assets serialized only _duration on the original script.
    # Use the real script GUID to test migration to the inherited backing field.
    meta = (navigation / 'FadeNavigationTransition.cs.meta').read_text(encoding='utf-8')
    guid = next(line.split(':', 1)[1].strip() for line in meta.splitlines() if line.startswith('guid:'))
    (project / 'Assets/LegacyFade.asset').write_text(f'''%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 0}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {guid}, type: 3}}
  m_Name: LegacyFade
  m_EditorClassIdentifier:
  _duration: 0.37
''', encoding='utf-8')
    log = project / 'Editor.log'
    print('Test artifacts: ' + str(project), flush=True)
    command = [str(engine), '-batchmode', '-nographics', '-projectPath', str(project),
               '-executeMethod', 'NavigationValidationBootstrap.Run', '-logFile', str(log)]
    flags = subprocess.CREATE_NO_WINDOW if os.name == 'nt' else 0
    try:
        result = subprocess.run(command, timeout=args.timeout, creationflags=flags)
        code = result.returncode
    except subprocess.TimeoutExpired:
        print('FAIL: Unity validation timed out. See ' + str(log), flush=True)
        return 1
    output = log.read_text(encoding='utf-8', errors='replace') if log.exists() else ''
    passed = code == 0 and 'NAVIGATION_VALIDATION_PASSED' in output
    print(('PASS' if passed else 'FAIL') + ' Unity Navigation validation', flush=True)
    if not passed:
        print(output[-14000:], flush=True)
    elif (project / 'results.json').exists():
        print((project / 'results.json').read_text(encoding='utf-8'), flush=True)
    return 0 if passed else 1


if __name__ == '__main__':
    raise SystemExit(main())
