### Print node

A print node allow you to print a message inside the log file.

For example, this can be used to see if a value is what you're waiting for, or just to do some test.

It has 2 inputs pin:

- Exec - This is where you need to link to another execution pin.
- Text - This is the message that will be shown. It need to be a string, if you need to log a int, float, or bool, check the converter node of those type.

It has 1 output pin:

- Exec - This is where you can link another node to continue the blueprint.

---

### Technical Information

Behind the scene, this node will generate a code that looks like:x

*Logger.Debug([TEXT]);*

It has some modification if the user is in debug mode:

```csharp
Guard.IsNotNull([TEXT]);
Logger.Debug([TEXT]);
```

