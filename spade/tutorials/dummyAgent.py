import spade


class DummyAgent(spade.agent.Agent):
    async def setup(self) -> None:
        print("Hello World! I'm agent {}".format(str(self.jid)))


async def main():
    agent = DummyAgent()
    await agent.start()


if __name__ == "__main__":
    spade.run(main())
