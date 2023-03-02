<script lang="ts">
	import { Accordion, Row } from 'carbon-components-svelte';

	import type { PageData } from './$types';

	import type { NatuerlichePersonEntry } from '$types';
	import { walter_get } from '$services/requests';
	import {
		Person,
		WalterGrid,
		WalterTextInput,
		Kontakte,
		Wohnungen,
		Vertraege,
		WalterHeaderDetail
	} from '$components';

	export let data: PageData;
	const url = `/api/kontakte/nat/${data.id}`;

	const a: Promise<NatuerlichePersonEntry> = walter_get(url);
	const entry: Partial<NatuerlichePersonEntry> = {};
	a.then((e) => Object.assign(entry, e));

	const title = a.then((x) => x.name);
</script>

<WalterHeaderDetail {a} {url} {entry} {title} />

<WalterGrid>
	<Row>
		<WalterTextInput
			bind:binding={entry.vorname}
			labelText="Vorname"
			value={a.then((x) => x.vorname)}
		/>
		<WalterTextInput
			bind:binding={entry.nachname}
			labelText="Nachname"
			value={a.then((x) => x.nachname)}
		/>
	</Row>
	<Person binding={entry} person={a} />

	<Accordion>
		<Kontakte
			title="Juristische Personen"
			rows={a.then((x) => x.juristischePersonen)}
		/>
		{#await a then x}
			<Wohnungen title="Wohnungen" rows={a.then((x) => x.wohnungen)} />
			<Vertraege title="Verträge" rows={a.then((x) => x.vertraege)} />
		{/await}
	</Accordion>
</WalterGrid>
