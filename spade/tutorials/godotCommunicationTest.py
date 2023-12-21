import spade


class MessageSenderAgent(spade.agent.Agent):

    behaviour = None

    class InformBehaviour(spade.behaviour.OneShotBehaviour):

        async def run(self) -> None:
            print("InformBehaviourRunning")
            message = spade.message.Message(to="edblaseTest1@jabbers.one")
            message.set_metadata("performative", "inform")
            message.set_metadata("ontology", "myOntology")
            message.set_metadata("language", "OWL-S")
            message.body = "Hello Receiver"

            await self.send(message)
            print("Message sent")

            self.exit_code = 0

            await self.agent.stop()

    async def setup(self) -> None:
        print("SenderAgent Started")
        self.behaviour = self.InformBehaviour()
        self.add_behaviour(self.behaviour)


async def main():
    sender_agent = MessageSenderAgent("edblase@jabbers.one", "6@GBcCqggzER@Gt")
    await sender_agent.start(auto_register=True)
    print("Sender Received")

    await spade.wait_until_finished(sender_agent)
    print("Agents finished")

if __name__ == "__main__":
    spade.run(main())
