%% Processing code for AoA estimation based on range FFTs of
% different receiveing antennas
%
% author: Christian Schöffmann
% University of Klagenfurt, Institute of Smart System Technologies
% 2021, July
%

function [target_range, aoa_grad] = aoa_estimation(IF_info, rvLocs, ranges, start_bin)

    NrTargets = length(rvLocs);
    
    if NrTargets > 0
        for i = 1:NrTargets
            desired_bin = start_bin + rvLocs(i); 
            target_range(i) = ranges(desired_bin);
            
            rvPh = unwrap(angle(IF_info(desired_bin,:)), []); 
            dPhi = diff(rvPh); 
            aoa_rad = (dPhi / (2*pi)) * ((3e8/57e9) /  0.0025);
            aoa_grad(i) = asind(aoa_rad);
        end
    else
        target_range = 3; 
        aoa_grad = 0;
    end
end