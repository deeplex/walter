<script lang="ts">
	import { Row } from 'carbon-components-svelte';
	import type { ComboBoxItem } from 'carbon-components-svelte/types/ComboBox/ComboBox.svelte';
	import {
		WalterComboBox,
		WalterGrid,
		WalterHeader,
		WalterTextInput
	} from '../../../components';
	import Adresse from '../../../components/Adresse.svelte';
	import { walter_get } from '../../../services/utils';
	import type { WohnungEntry } from '../../../types/wohnung.type';
	import type { PageData } from './$types';

	export let data: PageData;
	const a: Promise<WohnungEntry> = walter_get(`/api/wohnungen/${data.id}`);

	// TODO: See how many data is just not used... Maybe an extra controller?
	const kontakte: Promise<ComboBoxItem[]> = walter_get(
		`/api/selection/kontakte`
	);
</script>

<WalterHeader title={a.then((x) => x.anschrift)} />
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
</WalterGrid>
