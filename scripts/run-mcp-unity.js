#!/usr/bin/env node

const fs = require("fs");
const path = require("path");
const { spawn } = require("child_process");

const projectRoot = path.resolve(__dirname, "..");

const candidates = [
  path.join(projectRoot, "Packages", "com.gamelovers.mcp-unity", "Server~", "build", "index.js"),
];

const packageCacheRoot = path.join(projectRoot, "Library", "PackageCache");
if (fs.existsSync(packageCacheRoot)) {
  for (const entry of fs.readdirSync(packageCacheRoot)) {
    if (entry.startsWith("com.gamelovers.mcp-unity@")) {
      candidates.push(path.join(packageCacheRoot, entry, "Server~", "build", "index.js"));
    }
  }
}

const serverPath = candidates.find((candidate) => fs.existsSync(candidate));

if (!serverPath) {
  console.error("MCP Unity server build not found.");
  console.error("Expected one of:");
  for (const candidate of candidates) {
    console.error(`- ${candidate}`);
  }
  console.error("");
  console.error("Next steps:");
  console.error("1. Open this project in Unity.");
  console.error("2. Let Package Manager import com.gamelovers.mcp-unity.");
  console.error("3. In Unity, open Tools > MCP Unity > Server Window.");
  console.error("4. Click Force Install Server once if build/index.js has not been generated yet.");
  process.exit(1);
}

const child = spawn(process.execPath, [serverPath], {
  cwd: projectRoot,
  stdio: "inherit",
  env: process.env,
});

child.on("exit", (code, signal) => {
  if (signal) {
    process.kill(process.pid, signal);
    return;
  }

  process.exit(code ?? 0);
});
