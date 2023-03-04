<script lang="ts">
	import { Accordion, Row } from 'carbon-components-svelte';

	import type { PageData } from './$types';

	import type { WalterNatuerlichePersonEntry } from '$WalterTypes';
	import { walter_get } from '$WalterServices/requests';
	import {
		WalterPerson,
		WalterGrid,
		WalterTextInput,
		WalterKontakte,
		WalterWohnungen,
		WalterVertraege,
		WalterHeaderDetail
	} from '$WalterComponents';

	export let data: PageData;
	const url = `/api/kontakte/nat/${data.id}`;

	const a: Promise<WalterNatuerlichePersonEntry> = walter_get(url);
	const entry: Partial<WalterNatuerlichePersonEntry> = {};
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
	<WalterPerson binding={entry} person={a} />

	<Accordion>
		<WalterKontakte
			entry={{}}
			title="Juristische Personen"
			rows={a.then((x) => x.juristischePersonen)}
		/>
		{#await a then x}
			<WalterWohnungen
				entry={{}}
				title="Wohnungen"
				rows={a.then((x) => x.wohnungen)}
			/>
			<WalterVertraege
				entry={{}}
				title="Verträge"
				rows={a.then((x) => x.vertraege)}
			/>
		{/await}
	</Accordion>
</WalterGrid>
