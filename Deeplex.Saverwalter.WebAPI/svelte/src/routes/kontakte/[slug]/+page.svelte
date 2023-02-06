<script lang="ts">
	import { goto } from '$app/navigation';
	import { page } from '$app/stores';
	import { TextInputSkeleton } from 'carbon-components-svelte';
	import { JuristischePersonEntry, NatuerlichePersonEntry } from './classes';
	import JuristischePerson from './JuristischePerson.svelte';
	import NatuerlichePerson from './NatuerlichePerson.svelte';
	import Person from './Person.svelte';

	const request_options = {
		method: 'GET',
		headers: {
			'Content-Type': 'text/json'
		}
	};

	const async = fetch(`/api/kontakte/${window.location.href.split('/').at(-1)}`, request_options)
		.then((e) => e.json())
		.then((p) =>
			p.natuerlichePerson ? new NatuerlichePersonEntry(p) : new JuristischePersonEntry(p)
		);
</script>

<h1>
	{#await async}
		<Person />
	{:then person}
		{#if person.natuerlichePerson}
			<NatuerlichePerson {person} />
		{:else}
			<JuristischePerson {person} />
		{/if}
	{/await}
</h1>
