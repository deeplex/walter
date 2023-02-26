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
		Anhaenge
	} from '../../../../components';
	import type { WohnungEntry } from '../../../../types/wohnung.type';
	import type { VertragEntry } from '../../../../types/vertrag.type';

	export let data: PageData;
	const a: Promise<JuristischePersonEntry> = walter_get(
		`/api/kontakte/jur/${data.id}`
	);

	function getWohnungen(guid: string): Promise<WohnungEntry[]> {
		return walter_get(`/api/wohnungen/mieter/${guid}`);
	}

	function getVertraege(guid: string): Promise<VertragEntry[]> {
		return walter_get(`/api/vertraege/mieter/${guid}`);
	}
</script>

<WalterHeader title={a.then((x) => x.name)}>
	<Anhaenge rows={a.then((x) => x.anhaenge)} />
</WalterHeader>

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
		{#await a then x}
			<Wohnungen title="Wohnungen" rows={getWohnungen(x.guid)} />
			<Vertraege title="Verträge" rows={getVertraege(x.guid)} />
		{/await}
	</Accordion>
</WalterGrid>
