import spade

class SenderAgent(spade.agent.Agent):

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


class ReceiverAgent(spade.agent.Agent):
    class ReceiverBehaviour(spade.behaviour.OneShotBehaviour):

        async def run(self):
            print("Receiver Behaviour Running")

            message = await self.receive(1)
            if message:
                print("Message received with content :{}".format(message.body))
            else:
                print("No message received")

            await self.agent.stop()

    async def setup(self) -> None:
        print("ReceiverAgent started")
        behaviour = self.ReceiverBehaviour()
        template = spade.template.Template()
        template.set_metadata("performative","inform")
        self.add_behaviour(behaviour, template)

async def main():
    receiver_agent = ReceiverAgent("edblaseTest1@jabbers.one", "BD7ehX@UE2SURsQ")
    await receiver_agent.start(auto_register=True)
    print("Receiver Started")

    sender_agent = SenderAgent("edblase@jabbers.one", "6@GBcCqggzER@Gt")
    await sender_agent.start(auto_register=True)
    print("Sender Received")

    await spade.wait_until_finished(sender_agent)
    print("Agents finished")

if __name__ == "__main__":
    spade.run(main())
