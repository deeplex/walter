<script lang="ts">
	import { Accordion, Row } from 'carbon-components-svelte';
	import type { ComboBoxItem } from 'carbon-components-svelte/types/ComboBox/ComboBox.svelte';
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
		Anhaenge
	} from '../../../components';
	import Adresse from '../../../components/Adresse.svelte';
	import { walter_get } from '../../../services/utils';
	import type { WohnungEntry } from '../../../types/wohnung.type';
	import type { PageData } from './$types';

	export let data: PageData;
	const a: Promise<WohnungEntry> = walter_get(`/api/wohnungen/${data.id}`);

	const kontakte: Promise<ComboBoxItem[]> = walter_get(
		`/api/selection/kontakte`
	);
</script>

<WalterHeader title={a.then((x) => x.anschrift)}>
	<Anhaenge rows={a.then((x) => x.anhaenge)} />
</WalterHeader>
<WalterGrid>
	<Row>
		<WalterComboBox
			titleText="Besitzer"
			items={kontakte}
			selectedId={a.then((x) => x.besitzerId)}
		/>
	</Row>
	<Adresse adresse={a.then((x) => x.adresse)} />
	<Row>
		<WalterTextInput
			labelText="Besitzer"
			value={a.then((x) => x.bezeichnung)}
		/>
		<WalterTextInput
			labelText="Wohnfläche"
			value={a.then((x) => x.wohnflaeche)}
		/>
		<WalterTextInput
			labelText="Nutzfläche"
			value={a.then((x) => x.nutzflaeche)}
		/>
		<WalterTextInput labelText="Einheiten" value={a.then((x) => x.einheiten)} />
	</Row>
	<Row>
		<WalterTextInput labelText="Notiz" value={a.then((x) => x.notiz)} />
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
