## Synopsis

The Windows Service template exists to build a Windows Service that has the following capabilities:
* Background thread to do the actual work
* Periodic task framework to run tasks
* REST API to do work, can invoke a task
* TopShelf support for the implementation of the Windows Service
* Configuration support for multiple environments (Dev, Test, Prod, etc.)

The project has been named to TemplateService. To use simply rename "Template" to your name everywhere in the code and in files. 

## Motivation

I found that out of the box the Windows Service template from Visual Studio isn't near enough code to support basic things as mentioned above, so I created a template. 

## Contributors

If you would like to contribute to the template, please submit a pull request

## License

MIT License

Copyright (c) 2018 Greg Cummines

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.