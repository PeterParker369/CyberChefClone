<!DOCTYPE html>
<html>
<body>

<h1 align="center">
  <br>
  <a alt="CyberChef Clone" width="200"></a>
  <br>
  CyberChef Clone
  <br>
</h1>

<h4 align="center">CyberChef clone, built with <a href="https://dotnet.microsoft.com/" target="_blank">C#</a> and <a href="https://dotnet.microsoft.com/apps/aspnet" target="_blank">ASP.NET Core</a>. Inspired by <a href="https://github.com/gchq/CyberChef">GCHQ's CyberChef.</h4>

<p align="center">
  <a href="https://dotnet.microsoft.com/">
    <img src="https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet" alt=".NET">
  </a>
</p>

<p align="center">
  <a href="#about">About</a> •
  <a href="#features">Features</a> •
  <a href="#operations">Operations</a> •
  <a href="#installation">Installation</a> •
  <a href="#development">Development</a>
</p>

<br>

<h2 id="about">📖 About</h2>
<p>
  <strong>CyberChef Clone</strong> is a web application that provides a user-friendly interface for performing various "cyber" operations on data. It combines the power of a C# backend with a sleek, modern frontend to deliver a seamless experience for both technical and non-technical users.
</p>

<p align="center">IMG</p>

<br>

<h3>✨ Key Concepts</h3>
<ul>
  <li><strong>Recipe-based Processing</strong> — Chain multiple operations together to create complex data transformations</li>
  <li><strong>Real-time Execution</strong> — Results update automatically as you type (with optional manual mode)</li>
  <li><strong>Multi-format Support</strong> — Work with strings, hex, base64, and binary data</li>
  <li><strong>Extensible Architecture</strong> — Easy to add new operations</li>
</ul>
<br>

<h2 id="features">⚡ Features</h2>
<ul>
  <li>🔒 <strong>Client-side Security</strong> — All processing happens on the server</li>
  <li>⚡ <strong>Auto-Execution</strong> — Recipes execute automatically as you type (toggle on/off)</li>
  <li>💾 <strong>Save &amp; Load</strong> — Recipes persist in browser's localStorage</li>
  <li>📁 <strong>File Support</strong> — Drag-and-drop or upload files</li>
  <li>🎨 <strong>12 Color Themes</strong> — Customize the look and feel</li>
  <li>📋 <strong>Copy &amp; Save</strong> — Export results to clipboard or file</li>
  <li>📊 <strong>Execution Log</strong> — See detailed step-by-step results</li>
</ul>
<br>

<h2 id="operations">🔧 Operations</h2>
<h3>Encoding</h3>
<table>
  <tr>
    <th>Operation</th>
    <th>Description</th>
  </tr>
  <tr>
    <td><strong>Base64 Encode</strong></td>
    <td>Encode input to Base64 format</td>
  </tr>
  <tr>
    <td><strong>Base64 Decode</strong></td>
    <td>Decode Base64 input</td>
  </tr>
  <tr>
    <td><strong>Convert Encoding</strong></td>
    <td>Convert between character encodings (UTF-8, Windows-1251, etc.)</td>
  </tr>
</table>

<h3>Encryption</h3>
<table>
  <tr>
    <th>Operation</th>
    <th>Description</th>
  </tr>
  <tr>
    <td><strong>XOR</strong></td>
    <td>Apply XOR encryption with a key (0-255)</td>
  </tr>
  <tr>
    <td><strong>AES Encrypt</strong></td>
    <td>Encrypt data using AES (CBC, ECB, CFB, OFB modes)</td>
  </tr>
  <tr>
    <td><strong>AES Decrypt</strong></td>
    <td>Decrypt data using AES</td>
  </tr>
  <tr>
    <td><strong>PBKDF2</strong></td>
    <td>Generate key from password (RFC 2898)</td>
  </tr>
</table>

<h3>Hashing</h3>
<table>
  <tr>
    <th>Operation</th>
    <th>Description</th>
  </tr>
  <tr>
    <td><strong>SHA-256 Hash</strong></td>
    <td>Calculate SHA-256 hash</td>
  </tr>
  <tr>
    <td><strong>Hash</strong></td>
    <td>Hash with algorithm selection (MD5, SHA-1, SHA-256, SHA-384, SHA-512)</td>
  </tr>
</table>

<h3>Data Manipulation</h3>
<table>
  <tr>
    <th>Operation</th>
    <th>Description</th>
  </tr>
  <tr>
    <td><strong>Reverse</strong></td>
    <td>Reverse the input data</td>
  </tr>
  <tr>
    <td><strong>To Upper Case</strong></td>
    <td>Convert text to uppercase</td>
  </tr>
  <tr>
    <td><strong>To Lower Case</strong></td>
    <td>Convert text to lowercase</td>
  </tr>
  <tr>
    <td><strong>To Hex</strong></td>
    <td>Convert input to hexadecimal representation</td>
  </tr>
  <tr>
    <td><strong>From Hex</strong></td>
    <td>Convert hexadecimal to text or bytes</td>
  </tr>
</table>

<h3>Compression</h3>
<table>
  <tr>
    <th>Operation</th>
    <th>Description</th>
  </tr>
  <tr>
    <td><strong>Deflate</strong></td>
    <td>Compress data using Deflate algorithm (RFC 1951)</td>
  </tr>
  <tr>
    <td><strong>Inflate</strong></td>
    <td>Decompress data using Deflate algorithm</td>
  </tr>
</table>

<h2 id="installation">📦 Installation</h2>
<h3>Prerequisites</h3>
<ul>
  <li><a href="https://dotnet.microsoft.com/download">.NET 8.0 SDK</a></li>
  <li>Modern web browser (Chrome, Firefox, Edge)</li>
</ul>
<h3>Quick Start</h3>
<pre><code># Build and run
dotnet build
dotnet run

# Open in browser
# https://localhost:5001
</code></pre>

<h2 id="development">💻 Development</h2>
<h3>Adding a New Operation</h3>
<ol>
  <li>Create a new class in the <code>Operations</code> folder</li>
  <li>Implement the <code>IOperation</code> interface:</li>
</ol>

<pre><code>public class MyOperation : IOperation
{
    public string Name => "My Operation";
    public string Description => "Description of my operation";
    public string Category => "My Category";
    
    public Dictionary&lt;string, object&gt; Parameters { get; set; }
    public Dictionary&lt;string, ParameterInfo&gt; ParameterTypes { get; }

    public Task&lt;byte[]&gt; ExecuteAsync(byte[] input)
    {
        // Implementation here
        return Task.FromResult(result);
    }
}
</code></pre>

<ol start="3">
  <li>Register the operation in <code>OperationRegistry.cs</code></li>
  <li>Rebuild and test</li>
</ol>

<h3>Frontend Development</h3>

<ul>
  <li><strong>HTML</strong> — <code>wwwroot/index.html</code></li>
  <li><strong>CSS</strong> — <code>wwwroot/style.css</code> (supports 12 themes with glassmorphism)</li>
  <li><strong>JavaScript</strong> — <code>wwwroot/script.js</code> (vanilla JS, no frameworks)</li>
</ul>


<hr><p align="center">Built with ❤️</p><hr>

</body>
</html>