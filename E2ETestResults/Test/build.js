const child_process = require("child_process");
const path = require("path");

const isDebug = process.argv.indexOf("--release") < 0;
const compilerBinariesPath = path.resolve(__dirname, "..", "..", "test", "SaltarelleBinaries");

function getSourceFiles() {
  const relativeFileNames = ["Test.cs"];

  return relativeFileNames.map(file => path.normalize(file));
}

function getReferenceAssemblies() {
  const assemblies = [
    "mscorlib.dll",
    "NativeJsTypeDefs.dll",
    "jQuery.dll",
    "Web.dll",
    "TypeDefs.dll",
    "Underscore.dll"
  ];

  const runtimeAssemblyPath = path.resolve(compilerBinariesPath, "Runtime");
  return assemblies.map(assembly => path.resolve(runtimeAssemblyPath, assembly));
}

function getCommand() {
  const scexe = path.resolve(compilerBinariesPath, "Cli", "sc.dll");
  const options = `${isDebug ? "-debug" : ""} -w:0 -outasm:Test.dll -outscript:Test.js`;
  const refs = getReferenceAssemblies()
    .map(ref => `-r:${ref}`)
    .join(" ");
  const sourceFiles = getSourceFiles().join(" ");

  return `dotnet ${scexe} ${options} ${refs} ${sourceFiles}`;
}

console.log("Running Saltarelle compiler...");

child_process.exec(getCommand(), { cwd: __dirname }, (error, stdout, stderr) => {
  if (error) {
    console.error(`exec error: ${error}`);
  }

  console.log(stdout);
  console.error(stderr);
});
