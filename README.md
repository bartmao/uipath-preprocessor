# uipath-preprocessor
Preprocess the uipath workflow files and generate new ones using attributes in activity annotation.

一个基于.NET的UIPath工程预处理器。开发者以类似注解的方式定义预处理行为。预处理器根据注解生成目标工程文件。目前支持Activity重封装,  属性修改，生命周期和上下文管理等功能，同时开发了基于内存映射文件构建的循环队列，用于和UIPath运行时通信(未充分测试)。

## Sample
I have a UIPath sample project in the Sample folder, run 'Precompiler.bat', a folder 'Sample_Out' will be generated. All the wrappers will be added into the target workflow files
Compare 'HighlightExecution.xaml' or 'LogExecution.xaml' of different versions, you will see the effects.
In this specific sample, I provide two abilities(Defined in 'Bin\AttributesControllers.properties'):
1. Ability of logging(Start Executing/End Enecuting) for all Click activities, showing DisplayName of the activity
    > Click	+@Wrapper("Wrapper_LogExecution", $"@DisplayName")
2. Ability of highlight all click automations
    > Click	+@Wrapper("Wrapper_Highlight", $".//ui:Target/@Selector")

Before:

![image](https://user-images.githubusercontent.com/4489728/50966566-3e46d400-1510-11e9-91ee-3d1d87b26f08.png)

After:

![image](https://user-images.githubusercontent.com/4489728/50966510-1b1c2480-1510-11e9-8983-9b8fa0312a5d.png)


## Abilities
1. Wrappers (Most IMPORTANT one, other features can be created based on this)
> Target\t+@Wrapper(Wrapper Name, $"@DisplayName")
2. Workflow Extract
> @Workflow
3. Attribute Handler (A base class to modify attributes)
4. Global Timeout Control (A sample attribute handler based on the Attribute Handler)
> @Timeout(Seconds)


## In The End
This is a solution to give the ability to control non-functional requirements, UIPath projects are hard to maintain, hard to diagnose issues. 
Using attributes is a way to reduce complexities I learn from other languages.

I have another idea: Moving UIPath abilities completely to code(.NET/Java), I'm working on this idea and will show this in another repo.
