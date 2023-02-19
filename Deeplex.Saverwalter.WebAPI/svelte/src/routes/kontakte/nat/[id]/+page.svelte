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
		Vertraege
	} from '../../../../components';
	import type { WohnungListEntry } from '../../../../types/wohnunglist.type';
	import type { VertragListEntry } from '../../../../types/vertraglist.type';

	export let data: PageData;
	const a: Promise<NatuerlichePersonEntry> = walter_get(
		`/api/kontakte/nat/${data.id}`
	);

	function getWohnungen(guid: string): Promise<WohnungListEntry[]> {
		return walter_get(`/api/wohnungen/mieter/${guid}`);
	}

	function getVertraege(guid: string): Promise<VertragListEntry[]> {
		return walter_get(`/api/vertraege/mieter/${guid}`);
	}
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
		{#await a then x}
			<Wohnungen title="Wohnungen" rows={getWohnungen(x.guid)} />
			<Vertraege title="Verträge" rows={getVertraege(x.guid)} />
		{/await}
	</Accordion>
</WalterGrid>
