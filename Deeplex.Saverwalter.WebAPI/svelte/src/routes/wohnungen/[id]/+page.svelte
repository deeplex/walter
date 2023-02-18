<script lang="ts">
	import {
		Column,
		ComboBox,
		Grid,
		Row,
		TextInput,
		TextInputSkeleton
	} from 'carbon-components-svelte';
	import type { ComboBoxItem } from 'carbon-components-svelte/types/ComboBox/ComboBox.svelte';
	import Adresse from '../../../components/Adresse.svelte';
	import WalterComboBox from '../../../components/WalterComboBox.svelte';
	import WalterGrid from '../../../components/WalterGrid.svelte';
	import WalterHeader from '../../../components/WalterHeader.svelte';
	import WalterTextInput from '../../../components/WalterTextInput.svelte';
	import { request_options, walter_get } from '../../../services/utils';
	import type { KontaktListEntry } from '../../../types/kontaktlist.type';
	import type { WohnungEntry } from '../../../types/wohnung.type';
	import type { PageData } from './$types';

	export let data: PageData;
	const a: Promise<WohnungEntry> = fetch(
		`/api/wohnungen/${data.id}`,
		request_options
	).then((e) => e.json());

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
