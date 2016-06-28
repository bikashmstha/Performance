
var fs = require("fs")
var project = JSON.parse(fs.readFileSync("project.json"))

var precompileFlag = "precompile"
var mcjFlag = "mcj"

process.argv.forEach(function (val, index, array) {
    if (val.lastIndexOf(precompileFlag, 0) == 0)
    {
        var precompileVersion = val.substring(precompileFlag.length + 1)
        var dependencies = project["dependencies"];

        dependencies["Microsoft.AspNetCore.Mvc.Razor.Precompilation.Design"] = {
            "version" : precompileVersion + "-*",
            "target" : "package",
            "type" : "build"
        }

        if (!project["tools"])
        {
            project["tools"] = {}
        }

        project["tools"]["Microsoft.AspNetCore.Mvc.Razor.Precompilation.Tools"] = precompileVersion + "-*"

        if (!project["scripts"])
        {
            project["scripts"] = {}
        }

        if (!project["scripts"]["postpublish"])
        {
            project["scripts"]["postpublish"] = []
        }
        else if (typeof project["scripts"]["postpublish"] === "string")
        {
            project["scripts"]["postpublish"] = [ project["scripts"]["postpublish"] ]
        }

        project["scripts"]["postpublish"].push("dotnet razor-precompile --configuration %publish:Configuration% --framework %publish:TargetFramework% --output-path %publish:OutputPath% %publish:ProjectPath%")
    }
    else if (val.lastIndexOf(mcjFlag, 0) == 0)
    {
        var mcjParams = val.substring(mcjFlag.length + 1).split(",")
        var targetFramework = mcjParams[0]
        var sysRuntimeVersion = mcjParams[1]

        if (!project["buildOptions"])
        {
            project["buildOptions"] = {}
        }

        if (!project["buildOptions"]["define"])
        {
            project["buildOptions"]["define"] = []
        }

        project["buildOptions"]["define"].push("MCJ")

        if (!project["frameworks"]  || !project["frameworks"][targetFramework])
        {
            throw "Target framework " + targetFramework + " not declared."
        }

        if (!project["frameworks"][targetFramework]["dependencies"])
        {
            project["frameworks"][targetFramework]["dependencies"] = {}
        }

        if (project["frameworks"][targetFramework]["dependencies"]["System.Runtime.Loader"])
        {
            console.warn("Current project use System.Runtime.Loader " + project["frameworks"][targetFramework]["dependencies"]["System.Runtime.Loader"])
        }
        else
        {
            project["frameworks"][targetFramework]["dependencies"]["System.Runtime.Loader"]=sysRuntimeVersion
        }
    }
})

fs.writeFileSync("project.json", JSON.stringify(project, null, '\t'))
