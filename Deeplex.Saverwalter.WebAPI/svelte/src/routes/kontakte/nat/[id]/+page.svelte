<script lang="ts">
	import { Accordion, Row } from 'carbon-components-svelte';
	import type { PageData } from './$types';
	import type { NatuerlichePersonEntry } from '../../../../types/natuerlicheperson.type';
	import { walter_get } from '../../../../services/utils';
	import {
		Person,
		WalterHeader,
		WalterGrid,
		WalterTextInput,
		Anhaenge,
		Kontakte,
		Wohnungen,
		Vertraege,
		SaveWalter
	} from '../../../../components';
	import type { PersonEntry } from '../../../../types/person.type';

	export let data: PageData;
	const url = `/api/kontakte/nat/${data.id}`;

	const a: Promise<NatuerlichePersonEntry> = walter_get(url);
	const entry: Partial<NatuerlichePersonEntry> = {};
	a.then((e) => Object.assign(entry, e));
</script>

<WalterHeader title={a.then((x) => x.name)}>
	<SaveWalter {a} {url} body={entry} />
	<Anhaenge rows={a.then((x) => x.anhaenge)} />
</WalterHeader>

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
