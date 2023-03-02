<script lang="ts">
	import { Accordion, Row } from 'carbon-components-svelte';

	import type { PageData } from './$types';

	import {
		WalterGrid,
		WalterHeader,
		WalterTextInput,
		Betriebskostenrechnungen,
		Erhaltungsaufwendungen,
		Kontakte,
		Umlagen,
		Vertraege,
		Wohnungen,
		Zaehler
	} from '$components';
	import { walter_get } from '$services/requests';
	import type { AnhangEntry } from '$types';

	export let data: PageData;
	const a: Promise<AnhangEntry> = walter_get(`/api/anhaenge/${data.id}`);
	const entry: Partial<AnhangEntry> = {};
	a.then((e) => Object.assign(entry, e));
</script>

<WalterHeader title={a.then((x) => x.fileName)} />

<WalterGrid>
	<Row>
		<WalterTextInput
			bind:binding={entry.fileName}
			labelText="Dateiname"
			value={a.then((x) => x.fileName)}
		/>
		<WalterTextInput
			readonly
			labelText="Erstellungszeit"
			value={a.then((x) => x.creationTime)}
		/>
	</Row>
	<!-- <Row>
		<WalterTextInput labelText="Notiz" value={a.then((x) => x.notiz)} />
	</Row> -->
	<Accordion>
		<Betriebskostenrechnungen
			title="Betriebskostenrechnungen"
			rows={a.then((x) => x.betriebskostenrechnungen)}
		/>
		<Erhaltungsaufwendungen
			title="Erhaltungsaufwendungen"
			rows={a.then((x) => x.erhaltungsaufwendungen)}
		/>
		<Kontakte
			title="Kontakte"
			rows={a.then((x) => x.natuerlichePersonen.concat(x.juristischePersonen))}
		/>
		<Umlagen title="Umlagen" rows={a.then((x) => x.umlagen)} />
		<Vertraege title="Verträge" rows={a.then((x) => x.vertraege)} />
		<Wohnungen title="Wohnungen" rows={a.then((x) => x.wohnungen)} />
		<Zaehler title="Zähler" rows={a.then((x) => x.zaehler)} />
	</Accordion>
</WalterGrid>
