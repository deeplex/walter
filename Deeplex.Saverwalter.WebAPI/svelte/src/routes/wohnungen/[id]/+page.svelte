<script lang="ts">
	import { Accordion, Row } from 'carbon-components-svelte';
	import {
		WalterComboBox,
		WalterGrid,
		WalterHeader,
		WalterTextInput,
		Wohnungen,
		Vertraege,
		Zaehler,
		Erhaltungsaufwendungen,
		Betriebskostenrechnungen,
		Umlagen,
		Anhaenge,
		WalterNumberInput,
		SaveWalter
	} from '../../../components';
	import Adresse from '../../../components/Adresse.svelte';
	import { walter_get } from '../../../services/utils';
	import type { WohnungEntry } from '../../../types/wohnung.type';
	import type { PageData } from './$types';

	export let data: PageData;
	const url = `/api/wohnungen/${data.id}`;

	const a: Promise<WohnungEntry> = walter_get(url);
	const entry: Partial<WohnungEntry> = {};
	a.then((e) => Object.assign(entry, e));
</script>

<WalterHeader title={a.then((x) => 'TODO - ' + x.bezeichnung)}>
	<SaveWalter {a} {url} body={entry} />
	<Anhaenge rows={a.then((x) => x.anhaenge)} />
</WalterHeader>
<WalterGrid>
	<Row>
		<WalterComboBox
			bind:binding={entry.besitzer}
			titleText="Besitzer"
			api={`/api/selection/kontakte`}
			value={a.then((x) => x.besitzer)}
		/>
	</Row>
	<!-- TODO -->
	<Adresse adresse={a.then((x) => x.adresse)} />
	<Row>
		<WalterTextInput
			bind:binding={entry.bezeichnung}
			labelText="Bezeichnung"
			value={a.then((x) => x.bezeichnung)}
		/>
		<WalterNumberInput
			bind:binding={entry.wohnflaeche}
			label="Wohnfläche"
			value={a.then((x) => x.wohnflaeche)}
		/>
		<WalterNumberInput
			bind:binding={entry.nutzflaeche}
			label="Nutzfläche"
			value={a.then((x) => x.nutzflaeche)}
		/>
		<WalterNumberInput
			hideSteppers={false}
			bind:binding={entry.einheiten}
			label="Einheiten"
			value={a.then((x) => x.einheiten)}
		/>
	</Row>
	<Row>
		<WalterTextInput
			bind:binding={entry.notiz}
			labelText="Notiz"
			value={a.then((x) => x.notiz)}
		/>
	</Row>

	<Accordion>
		<Wohnungen title="Haus" rows={a.then((x) => x.haus)} />
		<Zaehler title="Zähler" rows={a.then((x) => x.zaehler)} />
		<Vertraege title="Verträge" rows={a.then((x) => x.vertraege)} />
		<Umlagen title="Umlagen" rows={a.then((x) => x.umlagen)} />
		<Betriebskostenrechnungen
			title="Betriebskostenrechnungen"
			rows={a.then((x) => x.betriebskostenrechnungen)}
		/>
		<Erhaltungsaufwendungen
			title="Erhaltungsaufwendungen"
			rows={a.then((x) => x.erhaltungsaufwendungen)}
		/>
	</Accordion>
</WalterGrid>
