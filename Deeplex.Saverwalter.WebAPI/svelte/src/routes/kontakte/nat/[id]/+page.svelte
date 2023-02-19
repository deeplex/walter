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
		Kontakte
	} from '../../../../components';

	export let data: PageData;
	const a: Promise<NatuerlichePersonEntry> = walter_get(
		`/api/kontakte/nat/${data.id}`
	);
</script>

<WalterHeader title={a.then((x) => x.name)}>
	<Anhaenge />
</WalterHeader>

<WalterGrid>
	<Row>
		<WalterTextInput labelText="Vorname" value={a.then((x) => x.vorname)} />
		<WalterTextInput labelText="Nachname" value={a.then((x) => x.nachname)} />
	</Row>
	<Person person={a} />

	<Accordion>
		<Kontakte
			title="Juristische Personen"
			rows={a.then((x) => x.juristischePersonen)}
		/>
		<!-- <Wohnungen title="Wohnungen" rows={a.then((x) => x.wohnungen)} /> -->
		<!-- <Vertraege title="Verträge" rows={a.then((x) => x.vertraege)} /> -->
	</Accordion>
</WalterGrid>
