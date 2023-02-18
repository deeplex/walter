<script lang="ts">
	import {
		Grid,
		Row,
		TextInput,
		TextInputSkeleton
	} from 'carbon-components-svelte';
	import type { PageData } from './$types';
	import type { ErhaltungsaufwendungEntry } from '../../../../types/erhaltungsaufwendung.type';
	import { walter_get } from '../../../../services/utils';
	import WalterDatePicker from '../../../../components/WalterDatePicker.svelte';
	import WalterComboBox from '../../../../components/WalterComboBox.svelte';
	import type { ComboBoxItem } from 'carbon-components-svelte/types/ComboBox/ComboBox.svelte';
	import WalterHeader from '../../../../components/WalterHeader.svelte';
	import WalterGrid from '../../../../components/WalterGrid.svelte';
	import WalterTextInput from '../../../../components/WalterTextInput.svelte';

	export let data: PageData;
	const a: Promise<ErhaltungsaufwendungEntry> = walter_get(
		`/api/erhaltungsaufwendungen/${data.id}`
	);

	const wohnungen: Promise<ComboBoxItem[]> = walter_get(
		'api/selection/wohnungen'
	);
</script>

<WalterHeader title={a.then((x) => x.aussteller + ' - ' + x.bezeichnung)} />

<WalterGrid>
	<Row>
		<WalterTextInput labelText="Typ" value={a.then((x) => x.bezeichnung)} />
		<WalterTextInput
			labelText="Aussteller"
			value={a.then((x) => x.aussteller.name)}
		/>
	</Row>
	<Row>
		<WalterComboBox
			titleText="Wohnung"
			selectedId={a.then((x) => x.wohnung.id.toString())}
			items={wohnungen}
		/>
		<WalterDatePicker value={a.then((x) => x.datum)} labelText="Datum" />
		<WalterTextInput labelText="Betrag" value={a.then((x) => x.betrag)} />
	</Row>
	<Row>
		<WalterTextInput labelText="Notiz" value={a.then((x) => x.notiz)} />
	</Row>
</WalterGrid>
