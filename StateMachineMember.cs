using System;
using Newtonsoft.Json;
using Stateless;

namespace BulkOperations
{
    public class StateMachineMember
    {
        public static void Run()
        {
            Console.WriteLine("Creating member from JSON");
            var member = FromJson("{ \"State\":\"1\",\"Name\":\"Jay\"}");

            Console.WriteLine($"Member {member.Name} created, membership state is {member.State}");

            member.Suspend();
            member.Reactivate();
            member.Terminate();

            Console.WriteLine("Member JSON:");

            var jsonString = member.ToJson();
            Console.WriteLine(jsonString);

            var anotherMember = FromJson(jsonString);

            if (member.Equals(anotherMember))
            {
                Console.WriteLine("Members are equal");
            }
        }

        private enum MemberTriggers
        {
            Suspend,
            Terminate,
            Reactivate
        }
        public enum MembershipState
        {
            Inactive,
            Active,
            Terminated
        }
        public MembershipState State => _stateMachine.State;
        public string Name { get; }

        private readonly StateMachine<MembershipState, MemberTriggers> _stateMachine;

        public StateMachineMember(string name)
        {
            _stateMachine = new StateMachine<MembershipState, MemberTriggers>(MembershipState.Active);
            Name = name;

            ConfigureStateMachine();
        }

        [JsonConstructor]
        private StateMachineMember(string state, string name)
        {
            var memberState = (MembershipState)Enum.Parse(typeof(MembershipState), state);
            _stateMachine = new StateMachine<MembershipState, MemberTriggers>(memberState);
            Name = name;

            ConfigureStateMachine();
        }

        private void ConfigureStateMachine()
        {
            _stateMachine.Configure(MembershipState.Active)
                .Permit(MemberTriggers.Suspend, MembershipState.Inactive)
                .Permit(MemberTriggers.Terminate, MembershipState.Terminated);

            _stateMachine.Configure(MembershipState.Inactive)
                .Permit(MemberTriggers.Reactivate, MembershipState.Active)
                .Permit(MemberTriggers.Terminate, MembershipState.Terminated);

            _stateMachine.Configure(MembershipState.Terminated)
                .Permit(MemberTriggers.Reactivate, MembershipState.Active);
        }

        public void Terminate()
        {
            _stateMachine.Fire(MemberTriggers.Terminate);
        }

        public void Suspend()
        {
            _stateMachine.Fire(MemberTriggers.Suspend);
        }

        public void Reactivate()
        {
            _stateMachine.Fire(MemberTriggers.Reactivate);
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static StateMachineMember FromJson(string jsonString)
        {
            return JsonConvert.DeserializeObject<StateMachineMember>(jsonString);
        }

        public bool Equals(StateMachineMember anotherMember)
        {
            return (State == anotherMember.State) && (Name == anotherMember.Name);
        }
    }
}
