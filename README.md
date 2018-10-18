# Secret Santa

After handing out who has who for this years secret santa, do you ever get sick of hearing "But I had John last year!"? 

Then boy howdy is this the app for you.

Let's say you have 8 people in your secret santa group. But wait! Some are married! You don't want to match two people that are married right?
They're probably getting each other enought gifts already.

What if they have a kid too? They shouldn't get him either. 

Or what if another 2 in the group are on the outs and you
don't want to risk one getting the other a box of coal?

Yeesh, this is getting complicated...

Enter this handy .NET console app!

## Configure Participants.json

Edit the participants.json file to contain everyone - as well as who shouldn't be matched with who.

So below Alex shouldn't match with Lauren, but can match with anyone else. Note you can add multiple people to the NeverMatchList.

```json
[
	{
		Name: "Alex",
		NeverMatchList: [
			{ Name: "Lauren" }
		]
	},
	{
		Name: "Lauren",
		NeverMatchList: [
			{ Name: "Alex" }
		]
	},
	{
		Name: "Micaela",
		NeverMatchList: [
			{ Name: "Bran" }
		]
	},
	{
		Name: "Bran",
		NeverMatchList: [
			{ Name: "Micaela" }
		]
	},
	{
		Name: "Kohl",
		NeverMatchList: [
			{ Name: "Jojo" }
		]
	},
	{
		Name: "Jojo",
		NeverMatchList: [
			{ Name: "Kohl" }
		]
	},
	{
		Name: "Mikkel",
		NeverMatchList: [
			{ Name: "Leif" }
		]
	},
	{
		Name: "Leif",
		NeverMatchList: [
			{ Name: "Mikkel" }
		]
	}
]
```

But wait! Last year Jojo matched with Leif, that's not shown in the participants.json file!

## Histories.json

Make an entry in HistoryParticipants for who each person got last year, and they won't match with that person again!

Note that anyone listed as a Gifter or a Giftee must exist in the participants.json file.

```json
[
	{
		Name: "2017",
		HistoryParticipants: [
			{
				Gifter: "Alex",
				Giftee: "Micaela"
			},
			{
				Gifter: "Lauren",
				Giftee: "Bran"
			},
			{
				Gifter: "Micaela",
				Giftee: "Jojo"
			},
			{
				Gifter: "Bran",
				Giftee: "Mikkel"
			},
			{
				Gifter: "Kohl",
				Giftee: "Leif"
			},
			{
				Gifter: "Jojo",
				Giftee: "Alex"
			},
			{
				Gifter: "Mikkel",
				Giftee: "Lauren"
			},
			{
				Gifter: "Leif",
				Giftee: "Kohl"
			}
		]
	}
]
```

## How to Run

This was written and tested in Visual Studio 2017 and .NET 4.7.1. It should also work with .NET Core, but I haven't tested that yet.

Run the app and you should see output like so:

```
Results
-------
Alex -> Leif
Lauren -> Jojo
Micaela -> Mikkel
Bran -> Lauren
Kohl -> Alex
Jojo -> Micaela
Mikkel -> Kohl
Leif -> Bran
Run again? [y]/n:
```

TADA!

Let everyone know who they have this year, then add these to histories.json and you're ready to forget all about this program until next year.

## Miscellaneous

You may wonder why it asks if you want to run it again. Well, due to the randomness sometimes it will leave people out, in which case you'll see:

```
FAILED!!
Results
-------
Alex ->
Lauren -> Jojo
Micaela -> Alex
Bran -> Leif
Kohl -> Bran
Jojo -> Mikkel
Mikkel -> Kohl
Leif -> Lauren
Run again? [y]/n:
```

Alex didn't get assigned someone, oh no! That happens sometimes, and the more conditions there are the more likely it'll happen, so just keep re-running it until everybody gets someone.

If there are any smart people reading this who know how to avoid this, let me know. I have one tweak in there to reduce the chances, but my graph-fu isn't good enough to eliminate it entirely.

