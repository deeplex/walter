<script lang="ts">
	import { Accordion, Row } from 'carbon-components-svelte';

	import type { PageData } from './$types';

	import type {
		WalterPersonEntry,
		WalterJuristischePersonEntry
	} from '$WalterTypes';
	import { walter_get } from '$WalterServices/requests';
	import {
		WalterKontakte,
		WalterPerson,
		WalterGrid,
		WalterTextInput,
		WalterWohnungen,
		WalterVertraege,
		WalterHeaderDetail
	} from '$WalterComponents';

	export let data: PageData;
	const url = `/api/kontakte/jur/${data.id}`;

	const a: Promise<WalterJuristischePersonEntry> = walter_get(url);
	const entry: Partial<WalterPersonEntry> = {};
	a.then((e) => Object.assign(entry, e));
</script>

<WalterHeaderDetail {a} {url} {entry} title={a.then((x) => x.name)} />

<WalterGrid>
	<Row>
		<WalterTextInput labelText="Bezeichnung" value={a.then((x) => x.name)} />
	</Row>
	<WalterPerson binding={entry} person={a} />

	<Accordion>
		<WalterKontakte title="Mitglieder" rows={a.then((x) => x.mitglieder)} />
		<WalterKontakte
			title="Juristische Personen"
			rows={a.then((x) => x.juristischePersonen)}
		/>
		{#await a then x}
			<WalterWohnungen title="Wohnungen" rows={a.then((x) => x.wohnungen)} />
			<WalterVertraege title="Verträge" rows={a.then((x) => x.vertraege)} />
		{/await}
	</Accordion>
</WalterGrid>
