# MonsterTradingCardsGame
Github-Link: https://github.com/matthiasmllnr/MonsterTradingCardsGame

## Design
Classic Server/Client Application.
Server listens on a specific address and starts each client which connects as a thread.
The incoming data gets structured and then invoked as an Event.
Then a simple switch statement handles the incoming requests and sends a reply to the client.
The data is persisted in a postgres database, which ensures via referential integrity that a user can only own cards that exists and a package/offer only consists of existing cards and so on with the user deck and stack.
The whole database gets mirrored as objects, so once this was done, it's pretty easy to write handle incoming requests and keep a persistent database state.

## Failures & Solutions
First I did not use JsonConvert to easily access the data given as a string. But later I ran into some problems by splitting them by my own.
So I replaced my own splitting function and used JsonConvert.

## Time spent
I started pretty late with the project on January the 4th, so it is not my best work and I know there are a lot of fix that could be optimized.
Since the January the 16th, I spent almost every day coding on the project. Around 2-4 hours a day.
