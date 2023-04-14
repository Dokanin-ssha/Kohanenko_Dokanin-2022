namespace SpaceBattle.Lib.Test;
using Moq;
using Xunit;
using System.Collections.Concurrent;

public class ServerTests{
    public ServerTests(){
        new InitScopeCommand().Execute();

        Mock<IReceiver> receiverMock = new();
        Mock<ICommand> commandMock = new();
        commandMock.Setup(obj=>obj.Execute());
        receiverMock.Setup(obj=>obj.Receive()).Returns(commandMock.Object);
        receiverMock.Setup(obj=>obj.IsEmpty()).Returns(false);
        Mock<ISender> senderMock = new();
        Mock<IStrategy> receiverStrategy = new();
        receiverStrategy.Setup(obj=>obj.UseStrategy()).Returns(receiverMock.Object);
        Mock<IStrategy> senderStrategy = new();
        senderStrategy.Setup(obj=>obj.UseStrategy()).Returns(senderMock.Object);

        IoC.Resolve<ICommand>("IoC.Add", "Game.Receiver", receiverStrategy.Object).Execute();
        IoC.Resolve<ICommand>("IoC.Add", "Game.Sender", senderStrategy.Object).Execute();
    }

    [Fact]
    public void Start_Void_Success(){
        IoC.Resolve<ICommand>("Game.CreateAndStartThreadCommand", 1).Execute();
        var thread = IoC.Resolve<ConcurrentDictionary<int, MyThread>>("Game.ThreadDictionary")[1];
        thread.Stop();
        thread.Start();
        Assert.True(true);
    }

    [Fact]
    public void CreateAndSTartThreadCommand_Action_Success(){
        Action action = () => {Assert.True(true);};
        IoC.Resolve<ICommand>("Game.CreateAndStartThreadCommand", 1, action).Execute();
    }

    [Fact]
    public void CreateAndSTartThreadCommand_ReceiverOrSenderNotAvailable_Error(){
        Mock<IStrategy> receiverStrategy = new();
        receiverStrategy.Setup(a=>a.UseStrategy()).Returns(new object());
        Mock<IStrategy> senderStrategy = new();
        senderStrategy.Setup(a=>a.UseStrategy()).Returns(new object());

        IoC.Resolve<ICommand>("IoC.Add", "Game.Receiver", receiverStrategy.Object).Execute();
        IoC.Resolve<ICommand>("IoC.Add", "Game.Sender", senderStrategy.Object).Execute();
        Action action = () => {Assert.True(true);};
        var cmd = IoC.Resolve<ICommand>("Game.CreateAndStartThreadCommand", 1, action);

        Assert.Throws<InvalidCastException>(()=>cmd.Execute());
    }

    [Fact]
    public void SendCommand_Cmd_Success(){
        Mock<ICommand> mockCommand = new();
        mockCommand.Setup(a=>a.Execute());

        Mock<ISender> senderMock = new();
        senderMock.Setup(obj=>obj.Send(It.IsAny<ICommand>())).Verifiable();
        Mock<IStrategy> senderStrategy = new();
        senderStrategy.Setup(obj=>obj.UseStrategy()).Returns(senderMock.Object);
        IoC.Resolve<ICommand>("IoC.Add", "Game.Sender", senderStrategy.Object).Execute();
        IoC.Resolve<ICommand>("Game.CreateAndStartThreadCommand", 2).Execute();

        IoC.Resolve<ICommand>("Game.SendCommand", 2, mockCommand.Object).Execute();

        senderMock.Verify(a=>a.Send(It.IsAny<ICommand>()));
    }

    [Fact]
    public void SendCommand_WrongID_Error(){
        Mock<ICommand> mockCommand = new();
        mockCommand.Setup(a=>a.Execute());

        Mock<ISender> senderMock = new();
        senderMock.Setup(obj=>obj.Send(It.IsAny<ICommand>()));
        Mock<IStrategy> senderStrategy = new();
        senderStrategy.Setup(obj=>obj.UseStrategy()).Returns(senderMock.Object);
        IoC.Resolve<ICommand>("IoC.Add", "Game.Sender", senderStrategy.Object).Execute();
        IoC.Resolve<ICommand>("Game.CreateAndStartThreadCommand", 2).Execute();

        var cmd = IoC.Resolve<ICommand>("Game.SendCommand", 3, mockCommand.Object);
        

        Assert.Throws<KeyNotFoundException>(() => cmd.Execute());
    }

    [Fact]
    public void ThreadHardStopCommand_Void_Success(){
        IoC.Resolve<ICommand>("Game.CreateAndStartThreadCommand", 1).Execute();
        Action action = ()=>{Assert.True(true);};
        IoC.Resolve<ICommand>("Game.HardStopThreadCommand", 1, action).Execute();
    }

    [Fact]
    public void ThreadHardStopCommand_Void_Error(){
        IoC.Resolve<ICommand>("Game.CreateAndStartThreadCommand", 1).Execute();
        Action action = ()=>{Assert.True(true);};
        var cmd = IoC.Resolve<ICommand>("Game.HardStopThreadCommand", 2, action);

        Assert.Throws<KeyNotFoundException>(() => cmd.Execute());
    }

    [Fact]
    public void ThreadSoftStopCommand_Void_Success(){
        IoC.Resolve<ICommand>("Game.CreateAndStartThreadCommand", 1).Execute();
        Action action = ()=>{Assert.True(true);};
        IoC.Resolve<ICommand>("Game.SoftStopThreadCommand", 1, action).Execute();
    }

    [Fact]
    public void ThreadSoftStopCommand_Void_Error(){
        IoC.Resolve<ICommand>("Game.CreateAndStartThreadCommand", 1).Execute();
        Action action = ()=>{Assert.True(true);};
        var cmd = IoC.Resolve<ICommand>("Game.SoftStopThreadCommand", 2, action);

        Assert.Throws<KeyNotFoundException>(() => cmd.Execute());
    }
}
