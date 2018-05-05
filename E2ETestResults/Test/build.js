const child_process = require('child_process');
const path = require('path');

const isDebug = process.argv.indexOf('--release') < 0;

function getSourceFiles() {
  const relativeFileNames = [
    'Test.cs',
  ];

  return relativeFileNames.map(file => path.normalize(file));
}

function getReferenceAssemblies() {
  const assemblies = [
    'mscorlib.dll',
    'NativeJsTypeDefs.dll',
    'Saltarelle.jQuery.dll',
    'Saltarelle.Web.dll',
    'TypeDefs.dll',
    'Underscore.dll',
  ];

  return assemblies.map(
    assembly => path.resolve(getCompilerPath(), assembly));
}

function getCompilerPath() {
  return path.resolve(__dirname, '..', '..', 'test', 'SaltarelleBinaries');
}

function getCommand() {
  const scexe = path.resolve(getCompilerPath(), 'sc.exe');
  const options = `${isDebug ? '-debug' : ''} -w:0 -outasm:Test.dll -outscript:Test.js`;
  const refs = getReferenceAssemblies().map(ref => `-r:${ref}`).join(' ');
  const sourceFiles = getSourceFiles().join(' ');
  return `${scexe} ${options} ${refs} ${sourceFiles}`;
}

console.log('Running Saltarelle compiler...');

child_process.exec(getCommand(), { cwd: __dirname }, (error, stdout, stderr) => {
  if (error) {
    console.error(`exec error: ${error}`);
  }

  console.log(stdout);
  console.error(stderr);
});
