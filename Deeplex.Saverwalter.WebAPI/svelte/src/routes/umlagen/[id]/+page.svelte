<script lang="ts">
	import { Accordion, Row } from 'carbon-components-svelte';

	import type { PageData } from './$types';

	import {
		Anhaenge,
		Betriebskostenrechnungen,
		SaveWalter,
		WalterGrid,
		WalterHeader,
		WalterTextInput,
		Wohnungen
	} from '$components';
	import { walter_get } from '$services/utils';
	import type { UmlageEntry } from '$types';

	export let data: PageData;
	const url = `/api/umlagen/${data.id}`;

	const a: Promise<UmlageEntry> = walter_get(url);
	const entry: Partial<UmlageEntry> = {};
	a.then((e) => Object.assign(entry, e));
</script>

<WalterHeader title={a.then((x) => x.typ + ' - ' + x.wohnungenBezeichnung)}>
	<SaveWalter {a} {url} body={entry} />
	<Anhaenge rows={a.then((x) => x.anhaenge)} />
</WalterHeader>
<WalterGrid>
	<Row>
		<!-- TODO: THESE ARE NO TEXT INPUTS, BUT MENUS... PLEASE CHANGE -->
		<WalterTextInput labelText="Typ" value={a.then((x) => x.typ)} />
		<WalterTextInput
			labelText="Wohnungen"
			value={a.then((x) => x.wohnungenBezeichnung)}
		/>
	</Row>
	<Row>
		<WalterTextInput
			labelText="Notiz"
			bind:binding={entry.notiz}
			value={a.then((x) => x.notiz)}
		/>
	</Row>

	<Accordion>
		<Wohnungen title="Wohnungen" rows={a.then((x) => x.wohnungen)} />
		<Betriebskostenrechnungen
			title="Betriebskostenrechnungen"
			rows={a.then((x) => x.betriebskostenrechnungen)}
		/>
	</Accordion>
</WalterGrid>
