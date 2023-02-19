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
		Vertraege
	} from '../../../../components';

	export let data: PageData;
	const a: Promise<JuristischePersonEntry> = walter_get(
		`/api/kontakte/jur/${data.id}`
	);
</script>

<WalterHeader title={a.then((e) => e.name)} />

<WalterGrid>
	<Row>
		<WalterTextInput labelText="Bezeichnung" value={a.then((x) => x.name)} />
	</Row>
	<Person person={a} />

	<Accordion>
		<Kontakte title="Mitglieder" rows={a.then((x) => x.mitglieder)} />
		<Kontakte
			title="Juristische Personen"
			rows={a.then((x) => x.juristischePersonen)}
		/>
		<!-- <Wohnungen title="Wohnungen" rows={a.then((x) => x.wohnungen)} /> -->
		<!-- <Vertraege title="Verträge" rows={a.then((x) => x.vertraege)} /> -->
	</Accordion>
</WalterGrid>
