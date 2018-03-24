const child_process = require('child_process');
const path = require('path');

function getSourceFiles() {
  const relativeFileNames = [
    'Bootstrap/LayoutMetrics.cs',
    'Bootstrap/MetricsController.cs',
    'Bootstrap/Utility.cs',
    'Core/Logging/BaseLogAppender.cs',
    'Core/Logging/ConsoleLogAppender.cs',
    'Core/Logging/ErrorTrace.cs',
    'Core/Logging/ILogAppender.cs',
    'Core/Logging/Logger.cs',
    'Core/Logging/MetricsLogger.cs',
    'Core/Logging/NavigationMetricsCollector.cs',
    'Core/Logging/WindowAppender.cs',
    'Core/Utility/BrowserSupport.cs',
    'Core/Utility/CssDictionary.cs',
    'Core/Utility/DomUtil.cs',
    'Core/Utility/DoubleUtil.cs',
    'Core/Utility/IBrowserViewport.cs',
    'Core/Utility/MiscUtil.cs',
    'Core/Utility/Param.cs',
    'Core/Utility/Point.cs',
    'Core/Utility/PointUtil.cs',
    'Core/Utility/RecordCast.cs',
    'Core/Utility/Rect.cs',
    'Core/Utility/Size.cs',
    'CoreSlim/ScriptEx.cs',
    'CoreSlim/WindowHelper.cs',
    'Properties/AssemblyInfo.cs',
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
  return path.resolve(__dirname, '..', '..', 'SaltarelleBinaries');
}

function getCommand() {
  const scexe = path.resolve(getCompilerPath(), 'sc.exe');
  const options = '-debug -w:0 -outasm:CoreSubset.dll -outscript:CoreSubset.js';
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
