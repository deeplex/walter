<script lang="ts">
	import {
		Button,
		Column,
		ComboBox,
		DatePicker,
		DatePickerInput,
		Grid,
		Row,
		TextInput,
		TextInputSkeleton
	} from 'carbon-components-svelte';
	import type { ComboBoxItem } from 'carbon-components-svelte/types/ComboBox/ComboBox.svelte';
	import WalterComboBox from '../../../components/WalterComboBox.svelte';
	import WalterDatePicker from '../../../components/WalterDatePicker.svelte';
	import { walter_get } from '../../../services/utils';
	import type { VertragEntry } from '../../../types/vertrag.type';
	import type { PageData } from './$types';

	export let data: PageData;

	const async: Promise<VertragEntry> = walter_get(`/api/vertraege/${data.id}`);

	const wohnungen: Promise<ComboBoxItem[]> = walter_get(
		`/api/selection/wohnungen`
	);
	const kontakte: Promise<ComboBoxItem[]> = walter_get(
		`/api/selection/kontakte`
	);

	let vermieter = () =>
		kontakte.then(async (e) => {
			const besitzer = await async.then((e) => e.wohnung.besitzerId);
			return e.find((f) => besitzer === f.id)?.text;
		});
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
	{:then x}
		<Row>
			<WalterDatePicker value={x.beginn} labelText="Beginn" />
			<WalterDatePicker value={x.ende} labelText="Ende" placeholder="Offen" />
		</Row>
		<Row>
			<WalterComboBox
				items={wohnungen}
				selectedId={x.wohnung.id.toString()}
				titleText="Wohnung"
			/>
			<!-- <Column>
				<div style="margin-top:0.75rem">
					<p class="bx--label">Vermieter:</p>
					{#await vermieter()}
						<TextInputSkeleton />
					{:then y}
						<p style="margin-top: 0.5rem" class=".bx--text-input::placeholder">
							{y}
						</p>
					{/await}
				</div>
			</Column> -->
			<WalterComboBox
				items={kontakte}
				selectedId={x.ansprechpartnerId}
				titleText="Ansprechpartner"
			/>
		</Row>
	{/await}
</Grid>
