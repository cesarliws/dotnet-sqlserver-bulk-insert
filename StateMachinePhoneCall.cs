using System;
using Stateless;
using Stateless.Graph;

namespace BulkOperations
{
    public class StateMachinePhoneCall
    {
        public static void Run()
        {
            var phoneCall = new StateMachinePhoneCall("Lokesh");

            phoneCall.Print();

            phoneCall.Dialed("Prameela");
            phoneCall.Print();

            phoneCall.Connected();
            phoneCall.Print();

            phoneCall.SetVolume(2);
            phoneCall.Print();

            phoneCall.Hold();
            phoneCall.Print();

            phoneCall.Mute();
            phoneCall.Print();

            phoneCall.Unmute();
            phoneCall.Print();

            phoneCall.Resume();
            phoneCall.Print();

            phoneCall.SetVolume(11);
            phoneCall.Print();


            Console.WriteLine(phoneCall.ToDotGraph());
        }
        enum Trigger
        {
            CallDialed,
            CallConnected,
            LeftMessage,
            PlacedOnHold,
            TakenOffHold,
            PhoneHurledAgainstWall,
            MuteMicrophone,
            UnmuteMicrophone,
            SetVolume
        }

        enum State
        {
            OffHook,
            Ringing,
            Connected,
            OnHold,
            PhoneDestroyed
        }

        State _state = State.OffHook;

        readonly StateMachine<State, Trigger> _machine;
        readonly StateMachine<State, Trigger>.TriggerWithParameters<int> _setVolumeTrigger;

        readonly StateMachine<State, Trigger>.TriggerWithParameters<string> _setCalleeTrigger;

        readonly string _caller;

        string _callee;

        public StateMachinePhoneCall(string caller)
        {
            _caller = caller;
            _machine = new StateMachine<State, Trigger>(() => _state, s => _state = s);

            _setVolumeTrigger = _machine.SetTriggerParameters<int>(Trigger.SetVolume);
            _setCalleeTrigger = _machine.SetTriggerParameters<string>(Trigger.CallDialed);

            _machine.Configure(State.OffHook)
                .Permit(Trigger.CallDialed, State.Ringing);

            _machine.Configure(State.Ringing)
                .OnEntryFrom(_setCalleeTrigger, callee => OnDialed(callee), "Caller number to call")
                .Permit(Trigger.CallConnected, State.Connected);

            _machine.Configure(State.Connected)
                .OnEntry(t => StartCallTimer())
                .OnExit(t => StopCallTimer())
                .InternalTransition(Trigger.MuteMicrophone, t => OnMute())
                .InternalTransition(Trigger.UnmuteMicrophone, t => OnUnmute())
                .InternalTransition(_setVolumeTrigger, (volume, t) => OnSetVolume(volume))
                .Permit(Trigger.LeftMessage, State.OffHook)
                .Permit(Trigger.PlacedOnHold, State.OnHold);

            _machine.Configure(State.OnHold)
                .SubstateOf(State.Connected)
                .Permit(Trigger.TakenOffHold, State.Connected)
                .Permit(Trigger.PhoneHurledAgainstWall, State.PhoneDestroyed);
        }

        void OnSetVolume(int volume)
        {
            Console.WriteLine("Volume set to " + volume + "!");
        }

        void OnUnmute()
        {
            Console.WriteLine("Microphone unmuted!");
        }

        void OnMute()
        {
            Console.WriteLine("Microphone muted!");
        }

        void OnDialed(string callee)
        {
            _callee = callee;
            Console.WriteLine("[Phone Call] placed for : [{0}]", _callee);
        }

        void StartCallTimer()
        {
            Console.WriteLine($"[Timer:] Call started at {DateTime.Now:G}");
        }

        void StopCallTimer()
        {
            Console.WriteLine($"[Timer:] Call ended at {DateTime.Now:G}");
        }

        public void Mute()
        {
            _machine.Fire(Trigger.MuteMicrophone);
        }

        public void Unmute()
        {
            _machine.Fire(Trigger.UnmuteMicrophone);
        }

        public void SetVolume(int volume)
        {
            _machine.Fire(_setVolumeTrigger, volume);
        }

        public void Print()
        {
            Console.WriteLine("[{1}] placed call and [Status:] {0}", _machine, _caller);
        }

        public void Dialed(string callee)
        {
            _machine.Fire(_setCalleeTrigger, callee);
        }

        public void Connected()
        {
            _machine.Fire(Trigger.CallConnected);
        }

        public void Hold()
        {
            _machine.Fire(Trigger.PlacedOnHold);
        }

        public void Resume()
        {
            _machine.Fire(Trigger.TakenOffHold);
        }

        public string ToDotGraph()
        {
            return UmlDotGraph.Format(_machine.GetInfo());
        }
    }
}