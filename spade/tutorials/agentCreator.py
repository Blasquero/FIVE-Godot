import spade


class CreatorAgent(spade.agent.Agent):
    async def setup(self) -> None:
        print(f"{self.jid} created.")


class CreateBehaviour(spade.behaviour.OneShotBehaviour):
    async def run(self):
        agent2 = CreatorAgent("edblase@jabbers.one", "6@GBcCqggzER@Gt")
        await agent2.start(auto_register=True)


async def main():
    agent1 = CreatorAgent("edblase@jabbers.one", "6@GBcCqggzER@Gt")
    behaviour = CreateBehaviour()
    agent1.add_behaviour(behaviour)
    await agent1.start(auto_register=True)

    # Wait until the behaviour is finished
    await behaviour.join()


if __name__ == "__main__":
    spade.run(main())
