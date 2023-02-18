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

	export let data: PageData;
	const async: Promise<ErhaltungsaufwendungEntry> = walter_get(
		`/api/erhaltungsaufwendungen/${data.id}`
	);

	const wohnungen: Promise<ComboBoxItem[]> = walter_get(
		'api/selection/wohnungen'
	);
</script>

<Grid>
	{#await async}
		<Row>
			<TextInputSkeleton />
			<TextInputSkeleton />
		</Row>
		<Row>
			<TextInputSkeleton />
			<TextInputSkeleton />
		</Row>
		<Row>
			<TextInputSkeleton />
		</Row>
	{:then x}
		<Row>
			<TextInput labelText="Typ" value={x.bezeichnung} />
			<TextInput labelText="Aussteller" value={x.aussteller.name} />
		</Row>
		<Row>
			<WalterComboBox
				titleText="Wohnung"
				selectedId={x.wohnung.id.toString()}
				items={wohnungen}
			/>
			<WalterDatePicker value={x.datum} labelText="Datum" />
			<TextInput labelText="Betrag" value={x.betrag} />
		</Row>
		<Row>
			<TextInput labelText="Notiz" value={x.notiz} />
		</Row>
	{/await}
</Grid>
