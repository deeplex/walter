<script lang="ts">
	import { Accordion, Row } from 'carbon-components-svelte';
	import type { PageData } from './$types';
	import type { JuristischePersonEntry } from '../../../../types/juristischeperson.type';
	import { walter_get } from '../../../../services/utils';
	import {
		Kontakte,
		Person,
		WalterHeader,
		WalterGrid,
		WalterTextInput,
		Wohnungen,
		Vertraege,
		Anhaenge,
		SaveWalter
	} from '../../../../components';
	import type { PersonEntry } from '../../../../types/person.type';

	export let data: PageData;
	const url = `/api/kontakte/jur/${data.id}`;

	const a: Promise<JuristischePersonEntry> = walter_get(url);
	const entry: Partial<PersonEntry> = {};
	a.then((e) => Object.assign(entry, e));
</script>

<WalterHeader title={a.then((x) => x.name)}>
	<SaveWalter {a} {url} body={entry} />
	<Anhaenge rows={a.then((x) => x.anhaenge)} />
</WalterHeader>

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
