<script lang="ts">
	import { Accordion, Row } from 'carbon-components-svelte';

	import type { PageData } from './$types';

	import type { PersonEntry, JuristischePersonEntry } from '$types';
	import { walter_get } from '$services/requests';
	import {
		Kontakte,
		Person,
		WalterGrid,
		WalterTextInput,
		Wohnungen,
		Vertraege,
		WalterHeaderDetail
	} from '$components';

	export let data: PageData;
	const url = `/api/kontakte/jur/${data.id}`;

	const a: Promise<JuristischePersonEntry> = walter_get(url);
	const entry: Partial<PersonEntry> = {};
	a.then((e) => Object.assign(entry, e));
</script>

<WalterHeaderDetail {a} {url} {entry} title={a.then((x) => x.name)} />

<WalterGrid>
	<Row>
		<WalterTextInput labelText="Bezeichnung" value={a.then((x) => x.name)} />
	</Row>
	<Person binding={entry} person={a} />

	<Accordion>
		<Kontakte title="Mitglieder" rows={a.then((x) => x.mitglieder)} />
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
