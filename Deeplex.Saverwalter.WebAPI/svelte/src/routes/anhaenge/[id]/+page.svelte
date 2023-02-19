<script lang="ts">
	import { Accordion, AccordionItem, Row } from 'carbon-components-svelte';
	import {
		WalterGrid,
		WalterHeader,
		WalterTextInput,
		WalterDatePicker,
		Betriebskostenrechnungen,
		Erhaltungsaufwendungen,
		Kontakte,
		Umlagen,
		Vertraege,
		Wohnungen,
		Zaehler
	} from '../../../components';
	import WalterAccordionItem from '../../../components/WalterDataWrapper.svelte';
	import { walter_get } from '../../../services/utils';
	import type { AnhangEntry } from '../../../types/anhang.type';
	import type { PageData } from './$types';

	export let data: PageData;
	const a: Promise<AnhangEntry> = walter_get(`/api/anhaenge/${data.id}`);
</script>

<WalterHeader title={a.then((x) => x.fileName)} />

<WalterGrid>
	<Row>
		<WalterTextInput labelText="Dateiname" value={a.then((x) => x.fileName)} />
		<WalterDatePicker
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
