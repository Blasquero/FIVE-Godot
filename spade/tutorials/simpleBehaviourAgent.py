import asyncio
import spade


class SimpleAgent(spade.agent.Agent):
    behaviour = None
    # Declaring new cyclic behaviour

    class CountingBehaviour(spade.behaviour.CyclicBehaviour):
        counter = 0

        # Called when the behaviour is created, before the main iteration of the behaviour
        async def on_start(self) -> None:
            print("Starting behaviour...")
            self.counter = 0

        # Called when the behaviour is ended
        async def on_end(self) -> None:
            print("Behaviour finished with exit code {}".format(self.exit_code))

        # Method called on each iteration of the behaviour. THIS METHOD IS AN ASYNC COROUTINE!
        async def run(self) -> None:
            print("Counter: {}".format(self.counter))
            self.counter += 1

            if self.counter > 10:
                # Stop the behaviour. Need to call return after calling, or it will continue execution this loop!
                # Exit code can be whatever type needed, can be checked with Behaviour.exit_code
                self.kill(exit_code=10)
                return

            await asyncio.sleep(1)

    async def setup(self) -> None:
        print("Starting Agent...")
        # Creating an instance of CountingBehaviour and adding it at the agent
        self.behaviour = self.CountingBehaviour()
        self.add_behaviour(self.behaviour)


async def main():
    agent = SimpleAgent("edblase@jabbers.one", "6@GBcCqggzER@Gt")
    await agent.start()
    print("Agent started")

    while not agent.behaviour.is_killed():
        try:
            await asyncio.sleep(1)
        except KeyboardInterrupt:
            break
    assert agent.behaviour.exit_code == 10

    await agent.stop()

if __name__ == "__main__":
    spade.run(main())
