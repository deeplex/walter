<script lang="ts">
	import { Accordion, Row } from 'carbon-components-svelte';
	import {
		Betriebskostenrechnungen,
		WalterGrid,
		WalterHeader,
		WalterTextInput
	} from '../../../components';
	import Wohnungen from '../../../components/lists/Wohnungen.svelte';
	import { walter_get } from '../../../services/utils';
	import type { UmlageEntry } from '../../../types/umlage.type';
	import type { PageData } from './$types';

	export let data: PageData;
	const a: Promise<UmlageEntry> = walter_get(`/api/umlagen/${data.id}`);
</script>

<WalterHeader title={a.then((x) => x.typ + ' - ' + x.wohnungenBezeichnung)} />

<WalterGrid>
	<Row>
		<WalterTextInput labelText="Bezeichnung" value={a.then((x) => x.typ)} />
		<WalterTextInput
			labelText="Wohnfläche"
			value={a.then((x) => x.wohnungenBezeichnung)}
		/>
	</Row>
	<Row>
		<WalterTextInput labelText="Notiz" value={a.then((x) => x.notiz)} />
	</Row>

	<Accordion>
		<Wohnungen title="Wohnungen" rows={a.then((x) => x.wohnungen)} />
		<Betriebskostenrechnungen
			title="Betriebskostenrechnungen"
			rows={a.then((x) => x.betriebskostenrechnungen)}
		/>
	</Accordion>
</WalterGrid>
